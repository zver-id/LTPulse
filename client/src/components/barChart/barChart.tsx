import { useGetFilteredMetricsQuery } from "../../storage/services/metrics-api.ts";
import loadingOrErrorScreen from "../../helpers/loadingOrErrorScreen.tsx";
import {Bar, BarChart, CartesianGrid, Legend, Tooltip, XAxis, YAxis} from "recharts";
import { RechartsDevtools } from '@recharts/devtools';

interface IBarChartProps {
    teamId: number;
    dayCount: number;
    filter: string;
}

function MetricBarChart({teamId, dayCount, filter}: IBarChartProps) {

    const {data, isLoading, isError}
        = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

    const loadingOrErrorResult = loadingOrErrorScreen({isLoading, isError})
    if (loadingOrErrorResult) {
        return loadingOrErrorResult;
    }

    return (
        <BarChart
            style={{width: '100%', maxWidth: '700px', maxHeight: '70vh', aspectRatio: 1.618}}
            responsive
            data={data}
            margin={{
                top: 5,
                right: 0,
                left: 0,
                bottom: 5,
            }}
        >
            <CartesianGrid strokeDasharray="3 3"/>
            <XAxis dataKey="day"/>
            <YAxis width="auto"/>
            <Tooltip/>
            <Legend/>
            <Bar dataKey="Поступившие" fill="#8884d8" activeBar={{fill: 'pink', stroke: 'blue'}} radius={[10, 10, 0, 0]}/>
            <Bar dataKey="Проработанные" fill="#82ca9d" activeBar={{fill: 'gold', stroke: 'purple'}} radius={[10, 10, 0, 0]}/>
            <RechartsDevtools/>
        </BarChart>
    )
}

export default MetricBarChart