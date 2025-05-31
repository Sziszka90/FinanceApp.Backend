import { TransactionTypeEnum } from "../Enums/TransactionType.enum";
import { Icon } from "../Icon/Icon";
import { Money } from "../Money/Money";

export interface CreateTransactionGroupDto {
    name: string,
    description?: string,
    icon?: Icon,
    limit?: Money,
}
