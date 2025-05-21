export interface Money {
    amount: number,
    currency: CurrencyEnum
}

export enum CurrencyEnum{
    USD,
    EUR,
    GBP,
    HUF,
    Unknown
}