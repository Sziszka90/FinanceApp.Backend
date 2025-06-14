import { FormControl } from "@angular/forms";
import { CurrencyEnum } from "../Money/Money";

export interface UserFormModel {
  currency: FormControl<CurrencyEnum | null>;
}
