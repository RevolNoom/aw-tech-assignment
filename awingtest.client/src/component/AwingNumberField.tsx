import { NumberField } from '@base-ui-components/react/number-field';

export function AwingNumberField(props: {
    title?: string,
    required?: boolean,
    disabled?: boolean,
    min?: number | null,
    max?: number | null,
    value?: number | null,
    onValueChange?: (value: number | null) => void,
    styleRoot?: React.CSSProperties,
    styleInput?: React.CSSProperties,
}) {
    return (
        <NumberField.Root
            className="AwingNumberFieldRoot"
            min={props.min ?? undefined}
            max={props.max ?? undefined}
            disabled={props.disabled}
            required={props.required}
            value={props.value}
            onValueChange={props.onValueChange}
            
            style={props.styleRoot}
        >
            {props.title &&
                <NumberField.ScrubArea>
                    <label>
                        {props.title}
                    </label>
                </NumberField.ScrubArea>}
                <NumberField.Group>
                    <NumberField.Input style={props.styleInput} />
                </NumberField.Group>
        </NumberField.Root>
    );
}
