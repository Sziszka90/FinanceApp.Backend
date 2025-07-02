import { CurrencyEnum } from "../Money/Money";

export interface UpdateUserDto {
    id: string,
    userName: string,
    password?: string,
    baseCurrency: CurrencyEnum
}
