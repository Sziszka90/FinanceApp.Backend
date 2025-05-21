import { Money } from "../Money/Money";


export interface CreateExpenseTransactionDto {
    name: string;
    description: string;
    value: Money;
    dueDate: Date;
    priority: number;
    transactionGroupId: string;
}
