import { Money } from "../Money/Money";
import { GetExpenseTransactionGroupDto } from "./GetExpenseTransactionGroupDto";

export interface GetExpenseTransactionDto {
    id: string;  
    name: string;
    description?: string;
    value: Money;
    dueDate?: string;  
    priority?: number;
    transactionGroup?: GetExpenseTransactionGroupDto;
}  
