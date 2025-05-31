import { Icon } from "../Icon/Icon";
import { Money } from "../Money/Money";

export interface GetTransactionGroupDto {
    id: string;
    name: string;
    description?: string;
    icon?: Icon;
    limit?: Money;
}


