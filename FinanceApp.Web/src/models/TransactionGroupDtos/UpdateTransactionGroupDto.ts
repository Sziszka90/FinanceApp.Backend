import { Icon } from "../Icon/Icon";
import { Money } from "../Money/Money";

export interface UpdateTransactionGroupDto {
    id: string,
    name: string,
    description?: string,
    groupIcon?: Icon,
    limit?: Money,
}
