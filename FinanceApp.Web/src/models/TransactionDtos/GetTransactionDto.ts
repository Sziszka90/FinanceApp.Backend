import { TransactionTypeEnum } from "../Enums/TransactionType.enum";
import { Money } from "../Money/Money";
import { GetTransactionGroupDto } from "../TransactionGroupDtos/GetTransactionGroupDto";

export interface GetTransactionDto {
    id: string;
    name: string;
    description?: string;
    value: Money;
    transactionDate: Date;
    transactionType: TransactionTypeEnum;
    transactionGroup?: GetTransactionGroupDto;
}
