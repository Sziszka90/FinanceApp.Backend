import { CurrencyEnum } from "../IncomeTransactionDtos/Money"

export interface GetUserDto {
    id: string,
    userName: string
    baseCurrency: CurrencyEnum
}