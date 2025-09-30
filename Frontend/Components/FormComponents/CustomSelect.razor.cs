using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;


namespace Frontend.Components.FormComponents;

public partial class CustomSelect<T> : ComponentBase where T : notnull
{
    
    [Parameter]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter]
    public string? Label { get; set; }

    [Parameter] public string Placeholder { get; set; } = "Choose";
    
    [Parameter] public IEnumerable<T> SelectedValues { get; set; } = [];
    [Parameter] public EventCallback<IEnumerable<T>> SelectedValuesChanged { get; set; }
    [Parameter] public Expression<Func<IEnumerable<T>>> SelectedValuesExpression { get; set; }
    [Parameter] public Dictionary<string, T> ValueMapping { get; set; } = new();
    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    private IJSObjectReference _module = null!;
    
    private DotNetObjectReference<CustomSelect<T>>? _dotNetObjectReference;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetObjectReference = DotNetObjectReference.Create(this);
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/FormComponents/CustomSelect.razor.js"); 
            await _module.InvokeVoidAsync("initComponent", _dotNetObjectReference, Id, ValueMapping);
        }
        
        
    }
    [JSInvokable]
    public async Task OnElementSelected(T[] element)
    {
        try
        {
            
           await SelectedValuesChanged.InvokeAsync(element);
            Console.WriteLine(string.Join(",", element));
            //Console.WriteLine(string.Join(",", ValueMapping.Keys));
            //var convertedElement = element.Select(e => ValueMapping[e]).ToList();
            
     
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}