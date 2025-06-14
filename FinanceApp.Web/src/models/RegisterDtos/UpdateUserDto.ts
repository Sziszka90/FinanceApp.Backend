import { CurrencyEnum } from "../Money/Money";

export interface UpdateUserDto {
    id: string,
    baseCurrency: CurrencyEnum
}
