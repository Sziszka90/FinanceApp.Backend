import { CurrencyEnum } from "../Money/Money";

export interface CreateUserDto {
    userName: string,
    email: string,
    password: string,
    baseCurrency: CurrencyEnum
}
