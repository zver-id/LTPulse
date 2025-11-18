import {CartesianGrid, Legend, Tooltip, XAxis, YAxis, Text, AreaChart, Area} from "recharts";
import type {LineChartProps} from "../../types/monthLineChart.ts";
import getAllLines from "../../helpers/getAllLines.ts";
import { useGetFilteredMetricsQuery } from "../../storage/services/metrics-api.ts"

import { Chart, registerables } from 'chart.js';
import { Line } from '';

Chart.register(...registerables);

const ZonesChart = ({ teamId, dayCount, filter }: LineChartProps) => {
    const { data, isLoading, isError } = useGetFilteredMetricsQuery({
        teamId, dayCount, filter
    });

    if (isLoading) return <h1>Loading...</h1>;
    if (isError) return <h1>Error...</h1>;
    const allMonths = getAllLines(data)
    const colors = [ '#E6194B', '#3CB44B', '#4363D8', '#F58231', '#911EB4', '#42D4F4',
        '#F032E6', '#BFEF45', '#FABED4', '#469990', '#DCBEFF', '#9A6324', '#800000'];

    const chartData = {
        labels: data?.map(item => item.day),
        datasets: allMonths?.map((month, index) => ({
            label: month,
            data: data?.map(item => Math.max(item[month], 0.6)), // Обрезаем снизу
            borderColor: colors[index % colors.length],
            backgroundColor: colors[index % colors.length] + '40',
            fill: true,
            tension: 0.4
        }))
    };

    const options = {
        scales: {
            y: {
                min: 0.6, // Обрезаем ось Y снизу
                max: 1,
                ticks: {
                    callback: (value: number) => `${(value * 100).toFixed(0)}%`
                }
            }
        }
    };

    return <Line data={chartData} options={options} />;
};

export default ZonesChart;