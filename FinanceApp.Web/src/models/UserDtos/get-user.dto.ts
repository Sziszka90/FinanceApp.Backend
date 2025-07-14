import { CurrencyEnum } from "../Money/money"

export interface GetUserDto {
    id: string,
    userName: string,
    email: string,
    baseCurrency: CurrencyEnum
}
