import {
    XAxis,
    YAxis,
    Tooltip,
    Legend,
    CartesianGrid,
    AreaChart,
    Area
} from 'recharts';

import type {LineChartProps} from "../../types/monthLineChart.ts";
import { useGetFilteredMetricsQuery } from "../../storage/services/metrics-api.ts"

function ZonesChart ({teamId, dayCount, filter} :LineChartProps) {

    const { data, isLoading, isError }
        = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

    if (isLoading) {
       return <h1>Loading...</h1>;
    }
    if (isError) {
       return <h1>Error...</h1>;
    }

    const colors = ['#3CB44B', '#F5DEB3', '#FFD700', '#E6194B'];

    return (
        <AreaChart
            style={{ width: '80%', aspectRatio: 1.618, maxHeight: '40vh' }}
            responsive
            data={data}
            stackOffset="expand"
            margin={{ top: 10, right: 20, left: 0, bottom: 0 }}
        >
            <defs>
                {/* Показываем только верхние 40% */}
                <clipPath id="cut40">
                    <rect x="0" y="0" width="100%" height="40%" />
                </clipPath>
            </defs>

            {/*
       transform-origin: top
       scaleY(2.5) = 1 / 0.4  → растягиваем оставшиеся 40% на 100%
    */}
            <g
                clipPath="url(#cut40)"
                style={{
                    transform: "scaleY(2.5)",
                    transformOrigin: "top"
                }}
            >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="day" orientation="top" />
                <YAxis width="auto" domain={[0,1]} />
                <Tooltip />
                <Area key={'0-8'}
                      dataKey="0-8"
                      type="monotone"
                      stroke={colors[0]}
                      fill={colors[0]}
                      stackId="1" />
                <Area key={'8-16'}
                      dataKey="8-16"
                      type="monotone"
                      stroke={colors[1]}
                      fill={colors[1]}
                      stackId="1" />
                <Area key={'16-24'}
                      dataKey="16-24"
                      type="monotone"
                      stroke={colors[2]}
                      fill={colors[2]}
                      stackId="1" />
                <Area key={'>24'}
                      dataKey=">24"
                      type="monotone"
                      stroke={colors[3]}
                      fill={colors[3]}
                      stackId="1" />
                <Legend
                    layout="vertical"
                    verticalAlign="middle"
                    align="right"
                    wrapperStyle={{ paddingLeft: 10 }}
                />

                {/*}
                {allLines?.map((line, index) => (
                    <Area
                        key={line}
                        dataKey={line}
                        type="monotone"
                        stroke={colors[index % colors.length]}
                        fill={colors[index % colors.length]}
                        stackId="1"
                    />
                ))}
                */}
            </g>
        </AreaChart>

    );
}

export default ZonesChart;