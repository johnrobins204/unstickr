using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using StoryFort.Models;
using StoryFort.Data;
using StoryFort.Services;
using System.Text.Json;
using System.Collections.Generic;

namespace StoryFort.Components.Pages
{
    public partial class Planner : IDisposable
    {
        [Parameter] public int StoryId { get; set; }
        [Inject] public StoryPersistenceService StoryPersistenceService { get; set; } = null!;
        [Inject] public StoryContext StoryContext { get; set; } = null!;
        [Inject] public ArchetypeService ArchetypeService { get; set; } = null!;
        
        private string CurrentArchetypeId { get; set; } = "hero";
        private ArchetypeDefinition? CurrentArchetype;
        private Story? CurrentStory;
        private StoryPlanData PlanData = new();
        
        private int SelectedPointId = -1;
        private string CurrentNoteText = "";
        private string SaveStatus = "";
        private bool ShowExamples = false;
        private System.Threading.Timer? _autoSaveTimer;
        
        private StoryPlotPoint? SelectedPointDefinition => 
            CurrentArchetype?.Points.FirstOrDefault(p => p.Id == SelectedPointId);

        protected override async Task OnInitializedAsync()
        {
            await LoadStory();
            _autoSaveTimer = new System.Threading.Timer(async _ => await InvokeAsync(SavePlan), null, 5000, 5000);
        }

        private async Task LoadStory()
        {
            CurrentStory = await StoryPersistenceService.LoadStoryAsync(StoryId);
            if (CurrentStory != null)
            {
                StoryContext.Title = CurrentStory.Title;
                StoryContext.StoryId = CurrentStory.Id;

                try 
                {
                    var metadata = await StoryPersistenceService.LoadMetadataAsync(StoryId);
                    if (!string.IsNullOrEmpty(metadata) && metadata != "{}")
                    {
                        PlanData = JsonSerializer.Deserialize<StoryPlanData>(metadata) ?? new StoryPlanData();
                    }
                }
                catch { }

                CurrentArchetypeId = PlanData.ArchetypeId ?? "hero";
                if (string.IsNullOrEmpty(CurrentArchetypeId)) CurrentArchetypeId = "hero";
                
                LoadArchetype(CurrentArchetypeId);
            }
        }

        private async Task OnArchetypeChanged()
        {
            LoadArchetype(CurrentArchetypeId);
            PlanData.ArchetypeId = CurrentArchetypeId;
            await SavePlan();
        }

        private void LoadArchetype(string id)
        {
            CurrentArchetype = ArchetypeService.GetArchetypes().FirstOrDefault(a => a.Id == id);
            // Fallback if not found
            if (CurrentArchetype == null)
            {
                CurrentArchetype = ArchetypeService.GetArchetypes().FirstOrDefault();
                if (CurrentArchetype != null) CurrentArchetypeId = CurrentArchetype.Id;
            }
        }

        private StoryPlotPoint? GetUserPoint(int pointId)
        {
            return PlanData.PlotPoints.FirstOrDefault(p => p.Id == pointId);
        }

        private void SelectPoint(int pointId)
        {
            if (SelectedPointId != -1)
            {
                UpdateLocalModel(SelectedPointId, CurrentNoteText);
            }

            SelectedPointId = pointId;
            ShowExamples = false;
            
            if (pointId != -1)
            {
                var userPoint = GetUserPoint(pointId);
                CurrentNoteText = userPoint?.Notes ?? "";
            }
        }
        
        private void ToggleExamples() => ShowExamples = !ShowExamples;

        private void UpdateLocalModel(int pointId, string text)
        {
             var pt = GetUserPoint(pointId);
             if (pt == null)
             {
                 pt = new StoryPlotPoint { Id = pointId };
                 PlanData.PlotPoints.Add(pt);
             }
             pt.Notes = text;
             pt.IsCompleted = !string.IsNullOrWhiteSpace(text);
        }

        private async Task SavePlan()
        {
             if (CurrentStory == null) return;
             
             if (SelectedPointId != -1)
             {
                UpdateLocalModel(SelectedPointId, CurrentNoteText);
             }

             SaveStatus = "Saving...";
             await InvokeAsync(StateHasChanged);
             
                try
                {
                    var metadata = JsonSerializer.Serialize(PlanData);
                    await StoryPersistenceService.SaveMetadataAsync(CurrentStory.Id, metadata);
                    SaveStatus = "Saved";
                }
                catch
                {
                    SaveStatus = "Error";
                }
             await InvokeAsync(StateHasChanged);
        }
        
        public void Dispose()
        {
            _autoSaveTimer?.Dispose();
        }
    }
}
