import { CurrencyEnum } from "../IncomeTransactionDtos/Money";

export interface UpdateUserDto {
    id: string,
    userName: string,
    password: string,
    baseCurrency: CurrencyEnum
}