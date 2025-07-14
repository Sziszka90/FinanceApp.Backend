import { Money } from "../Money/money";

export interface GetTransactionGroupDto {
    id: string;
    name: string;
    description?: string;
    groupIcon?: string;
}
