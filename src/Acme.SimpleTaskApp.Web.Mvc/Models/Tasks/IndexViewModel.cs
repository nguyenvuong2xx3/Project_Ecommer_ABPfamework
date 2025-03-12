using Acme.SimpleTaskApp.Tasks.Dtos;
using Acme.SimpleTaskApp.Tasks;
using System.Collections.Generic;
using Abp.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Acme.SimpleTaskApp.Web.Models.Tasks
{
    public class IndexViewModel
    {
        public IReadOnlyList<TaskListDto> Tasks { get; }

        public IndexViewModel(IReadOnlyList<TaskListDto> tasks)
        {
            Tasks = tasks;
        }

        public string GetTaskLabel(TaskListDto task)
        {
            switch (task.State)
            {
                case TaskState.Open:
                    return "label-success";
                default:
                    return "label-default";
            }
        }

        public TaskState? SelectedTaskState { get; set; }

        public List<SelectListItem> GetTasksStateSelectListItems(ILocalizationManager localizationManager)
        {
            var list = new List<SelectListItem>
        {
            new SelectListItem
            {
                Text = localizationManager.GetString(SimpleTaskAppConsts.LocalizationSourceName, "AllTasks"),
                Value = "",
                Selected = SelectedTaskState == null
            }
        };

            list.AddRange(Enum.GetValues(typeof(TaskState))
                    .Cast<TaskState>()
                    .Select(state =>
                        new SelectListItem
                        {
                            Text = localizationManager.GetString(SimpleTaskAppConsts.LocalizationSourceName, $"TaskState_{state}"),
                            Value = state.ToString(),
                            Selected = state == SelectedTaskState
                        })
            );

            return list;
        }
    }
}
