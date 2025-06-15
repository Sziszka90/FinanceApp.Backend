import { AbstractControl, ValidationErrors } from "@angular/forms";

export function enumValidator(enumObj: any) {
  return (control: AbstractControl): ValidationErrors | null => {
    // Accept both direct value and object with value property
    const value =
      control.value && typeof control.value === "object" && "value" in control.value
        ? control.value.value
        : control.value;
    const isValid = Object.values(enumObj).includes(value);
    return isValid ? null : { invalidEnum: true };
  };
}
