import { Money } from "../Money/Money";

export interface GetExpenseTransactionGroupDto {
    id: string;  
    name: string;
    description?: string;  
    icon?: string; 
    limit?: Money;
}