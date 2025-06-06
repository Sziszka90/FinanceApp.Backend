import { Money } from "../Money/Money";

export interface GetTransactionGroupDto {
    id: string;
    name: string;
    description?: string;
    groupIcon?: string;
    limit?: Money;
}
