using Microsoft.AspNetCore.Components.Forms;

namespace SuggestionAppUI.Components;

// we are creating this component to by pass EditForm in Components and to override InputRadioGroup 
// we bypass double click scenario with this component otherwise we would have to double click to change the radiobutton
public class MyInputRadioGroup<TValue> : InputRadioGroup<TValue>
{
    private string _name;
    private string _fieldClass;

    protected override void OnParametersSet()
    {
        var fieldClass = EditContext?.FieldCssClass(FieldIdentifier) ?? string.Empty;
        if (fieldClass != _fieldClass || Name != _name)
        {
            _fieldClass = fieldClass;
            _name = Name;
            base.OnParametersSet();
        }// this is going to only execute this if we have a change in field class or name

    }
}
