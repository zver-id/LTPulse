import {CartesianGrid, Legend, Line, LineChart, Tooltip, XAxis, YAxis, Text } from "recharts";
import type {LineChartProps} from "../../types/monthLineChart.ts";

import { useGetFilteredMetricsQuery } from "../../storage/services/metrics-api.ts"
import getAllLines from "../../helpers/getAllLines.ts"

function MonthLineChart ({teamId, dayCount, filter} :LineChartProps) {

    const { data, isLoading, isError }
        = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

    if (isLoading) {
        return <h1>Loading...</h1>;
    }
    if (isError) {
        return <h1>Error...</h1>;
    }

    const allMonths = getAllLines(data)
    const colors = [ '#E6194B', '#3CB44B', '#4363D8', '#F58231', '#911EB4', '#42D4F4',
        '#F032E6', '#BFEF45', '#FABED4', '#469990', '#DCBEFF', '#9A6324', '#800000'];

    return (
        <LineChart
            style={{ width: '80%', aspectRatio: 1.618, maxHeight: '40vh' }}
            margin={{ top: 5, right: 20, bottom: 5, left: 0 }}
            responsive
            data={data}>
            <Text x={250} y={20} textAnchor="middle" dominantBaseline="middle">
                Заголовок графика
            </Text>
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