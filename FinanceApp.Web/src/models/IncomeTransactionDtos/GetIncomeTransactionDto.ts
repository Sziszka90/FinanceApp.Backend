import { GetIncomeTransactionGroupDto } from "./GetIncomeTransactionGroupDto";
import { Money } from "./Money";

export interface GetIncomeTransactionDto {
    id: string;  
    description?: string;
    value: Money;
    dueDate?: string;  
    transactionGroup?: GetIncomeTransactionGroupDto;
}  