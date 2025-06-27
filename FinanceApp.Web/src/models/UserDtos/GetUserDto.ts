import { CurrencyEnum } from "../Money/Money"

export interface GetUserDto {
    id: string,
    userName: string,
    email: string,
    baseCurrency: CurrencyEnum
}
