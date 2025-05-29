import { Money } from "../Money/Money";
import { GetTransactionGroupDto } from "./GetTransactionGroupDto";

export interface GetTransactionDto {
    id: string;
    name: string;
    description?: string;
    value: Money;
    transactionDate: Date;
    transactionType: TransactionTypeEnum;
    transactionGroup?: GetTransactionGroupDto;
}
