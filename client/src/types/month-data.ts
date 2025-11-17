type DynamicMonthKeys = `${
    | 'January'
    | 'February'
    | 'March'
    | 'April'
    | 'May'
    | 'June'
    | 'July'
    | 'August'
    | 'September'
    | 'October'
    | 'November'
    | 'December'} ${number}`;

export type MonthData = {
    day: string;
    total: number;
    [key: DynamicMonthKeys]: number;
}
