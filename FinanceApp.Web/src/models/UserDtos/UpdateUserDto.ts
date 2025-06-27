import { CurrencyEnum } from "../Money/Money";

export interface UpdateUserDto {
    id: string,
    userName: string,
    email: string,
    password?: string,
    baseCurrency: CurrencyEnum
}
