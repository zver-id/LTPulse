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

export type dayData = {
    day: string;
    total: number;
    [key: DynamicMonthKeys]: number;
}
const aurora: Array<dayData> = [
    {
        day: "25.10.2025",
        total: 16,
        "April 2025": 12,
        "May 2025": 4,
    },
    {
        day: "26.10.2025",
        total: 16,
        "April 2025": 11,
        "May 2025": 5,
    },
    {
        day: "27.10.2025",
        total: 18,
        "April 2025": 10,
        "May 2025": 7,
    }
]

const atlas: Array<dayData> = [
    {
        day: "25.10.2025",
        total: 13,
        "April 2025": 22,
        "May 2025": 15,
    },
    {
        day: "26.10.2025",
        total: 34,
        "April 2025": 32,
        "May 2025": 51,
    },
    {
        day: "27.10.2025",
        total: 48,
        "April 2025": 56,
        "May 2025": 42,
    }
]

export function getData(team: string):(Array<dayData>) {
    switch (team) {
        case 'aurora':
            return aurora
        case 'atlas':
            return atlas
        default:
            return atlas
    }
}