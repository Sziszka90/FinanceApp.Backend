import { CurrencyEnum } from "../Money/Money";

export interface CreateUserDto {
    userName: string,
    password: string,
    baseCurrency: CurrencyEnum
}