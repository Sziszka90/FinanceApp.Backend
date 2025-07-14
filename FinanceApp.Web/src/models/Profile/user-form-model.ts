import { FormControl } from "@angular/forms";
import { CurrencyEnum } from "../Money/money";

export interface UserFormModel {
  userName: FormControl<string | null>;
  password: FormControl<string | null>;
  currency: FormControl<CurrencyEnum | null>;
}
