using System;
using System.Linq;
using System.Threading.Tasks;
using Blazored.TextEditor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;
using StoryFort.Models;
using Microsoft.EntityFrameworkCore;
using StoryFort.Services;
using StoryFort.Data;
using System.Collections.Generic;

namespace StoryFort.Components.Pages
{
    public partial class Editor
    {
        [Parameter] public int StoryId { get; set; }
        [Inject] public StoryPersistenceService StoryPersistenceService { get; set; } = null!;
        [Inject] public SessionState Session { get; set; } = null!;
        [Inject] public StoryContext StoryContext { get; set; } = null!;

        BlazoredTextEditor? QuillHtml;
        string SaveStatus = "Ready";
        string InitialContent = "";
        int WordCount = 0;
        bool IsNotebookOpen = false;
        private bool HeaderExpanded = true;
        private DotNetObjectReference<Editor>? _objRef;
        private IJSObjectReference? _editorModule;
        private Account? CurrentAccount;

        private void ToggleHeader() => HeaderExpanded = !HeaderExpanded;
        private void ToggleNotebook() => IsNotebookOpen = !IsNotebookOpen;

        private async Task OnFontChange(ChangeEventArgs e)
        {
            var fontClass = e.Value?.ToString();
            if (!string.IsNullOrWhiteSpace(fontClass))
            {
                await JS.InvokeVoidAsync("setBodyClass", $"h-screen flex {fontClass}");
                Session.UpdateThemePreference(p => p.FontClass = fontClass);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var story = await StoryPersistenceService.LoadStoryAsync(StoryId);
                if (story != null)
                {
                    CurrentAccount = story.Account;
                    StoryContext.Account = story.Account;

                    if (!string.IsNullOrEmpty(story.Account?.ThemePreferenceJson))
                    {
                        try
                        {
                            var prefs = System.Text.Json.JsonSerializer.Deserialize<ThemePreference>(story.Account.ThemePreferenceJson);
                            if (prefs != null)
                            {
                                Session.UpdateThemePreference(_ => {
                                    Session.ThemePreference = prefs;
                                });
                            }
                        }
                        catch { }
                    }

                    StoryContext.Title = story.Title;
                    StoryContext.StoryId = story.Id;
                    StoryContext.Content = story.Content;
                    StoryContext.Genre = story.Genre;

                    if (story.Account?.ActiveTheme != null)
                    {
                        Session.CurrentTheme = story.Account.ActiveTheme;
                        Session.NotifyStateChanged();
                    }

                    InitialContent = story.Content;

                    var notebooks = story.Account?.Notebooks.ToList() ?? new List<Notebook>();
                    Session.Notebooks = notebooks;
                    
                    var linkedIds = story.EntityLinks.Select(l => l.NotebookEntityId).ToHashSet();
                    StoryContext.LinkedEntityIds = linkedIds;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading story {StoryId}", StoryId);
                SaveStatus = "Error Loading";
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this);
                _editorModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/editor.js");
                await _editorModule.InvokeVoidAsync("initAutoSave", _objRef, 2000);
            }
        }

        [JSInvokable]
        public async Task TriggerAutoSave()
        {
            await InvokeAsync(async () =>
            {
                await SaveContent();
                StateHasChanged();
            });
        }

        private async Task SaveContent()
        {
            if (QuillHtml == null) return;

            SaveStatus = "Saving...";
            StateHasChanged();

            try
            {
                string html = await QuillHtml.GetHTML();
                StoryContext.Content = html;

                string text = await QuillHtml.GetText();
                WordCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

                await StoryPersistenceService.SaveContentAsync(StoryId, html, StoryContext.Title, StoryContext.Genre);
                SaveStatus = "Saved";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving content");
                SaveStatus = "Not Saved!";
            }
        }

        public async ValueTask DisposeAsync()
        {
            _objRef?.Dispose();
            if (_editorModule != null)
            {
                await _editorModule.DisposeAsync();
            }
        }
    }
}
