import { CurrencyEnum } from "../Money/money";

export interface CreateUserDto {
    userName: string,
    email: string,
    password: string,
    baseCurrency: CurrencyEnum
}
