import { CurrencyEnum } from "../Money/money";

export interface UpdateUserDto {
    id: string,
    userName: string,
    password?: string,
    baseCurrency: CurrencyEnum
}
