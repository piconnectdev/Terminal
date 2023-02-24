using Microsoft.AspNetCore.Components;
using Schedule.RunnerSpace;
using System;
using System.Threading.Tasks;

namespace Terminal.Client.Components
{
  public class BaseComponent : ComponentBase
  {
    /// <summary>
    /// Updater
    /// </summary>
    protected virtual TimeRunner Updater { get; set; } = new() { Count = 1, Span = TimeSpan.FromMilliseconds(100) };

    /// <summary>
    /// Render
    /// </summary>
    protected virtual Task Render(Action action)
    {
      return Updater.Send(() =>
      {
        action();
        InvokeAsync(StateHasChanged);

      }).Task;
    }
  }
}
