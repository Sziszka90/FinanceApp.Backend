import { CurrencyEnum } from "../IncomeTransactionDtos/Money";

export interface CreateUserDto {
    userName: string,
    password: string,
    baseCurrency: CurrencyEnum
}