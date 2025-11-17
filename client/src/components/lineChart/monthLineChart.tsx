import {CartesianGrid, Legend, Line, LineChart, Tooltip, XAxis, YAxis} from "recharts";
import type {LineChartProps} from "../../types/monthLineChart.ts";
import type { MonthData } from "../../types/month-data.ts"
import { useGetAllMetricsQuery } from "../../storage/services/metrics-api.ts"

function MonthLineChart ({teamId, dayCount} :LineChartProps) {

    const getAllMonths = (teamData: Array<MonthData> | undefined):Array<string> => {
        const months = new Set<string>();
        teamData?.forEach(item => {
            Object.keys(item).forEach(key => {
                if (key !== 'day') {
                    months.add(key);
                }
            });
        });
        return Array.from(months);
    };

    const { data, isLoading, isError } = useGetAllMetricsQuery({teamId: teamId, dayCount: dayCount});

    if (isLoading) {
        return <h1>Loading...</h1>;
    }
    if (isError) {
        return <h1>Error...</h1>;
    }

    const allMonths = getAllMonths(data)
    const colors = [ '#E6194B', '#3CB44B', '#4363D8', '#F58231', '#911EB4', '#42D4F4',
        '#F032E6', '#BFEF45', '#FABED4', '#469990', '#DCBEFF', '#9A6324', '#800000'];

    return (
        <LineChart
            style={{ width: '80%', aspectRatio: 1.618, maxHeight: '40vh' }}
            margin={{ top: 5, right: 20, bottom: 5, left: 0 }}
            responsive
            data={data}>
            <XAxis dataKey="day" />
            <YAxis width="auto" />
            <CartesianGrid stroke="#aaa" strokeDasharray="5 5" />
            {allMonths?.map((month, index) =>{
                return(
                    <Line key={month}
                          dataKey={month}
                          stroke={colors[index % colors.length]}
                          type="monotone"
                    />
                )
            })}
            <Legend />
            <Tooltip />
        </LineChart>

    );
}

export default MonthLineChart