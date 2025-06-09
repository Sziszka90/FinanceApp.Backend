import { Money } from "../Money/Money";

export interface UpdateTransactionGroupDto {
    id: string,
    name: string,
    description?: string,
    groupIcon?: string,
    limit?: Money,
}

