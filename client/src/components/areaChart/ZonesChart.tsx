import {
    XAxis,
    YAxis,
    Tooltip,
    Legend,
    CartesianGrid,
    AreaChart,
    Area
} from 'recharts';

import type {ZonesChartProps} from "../../types/zones-chart-props.ts";
import { useGetFilteredMetricsQuery } from "../../storage/services/metrics-api.ts"

function ZonesChart ({teamId, dayCount, filter, zoneColor} :ZonesChartProps) {

    const { data, isLoading, isError }
        = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

    if (isLoading) {
       return <h1>Loading...</h1>;
    }
    if (isError) {
       return <h1>Error...</h1>;
    }

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
                {zoneColor?.map((zone) => (
                    <Area
                        key={zone.name}
                        dataKey={zone.name}
                        type="monotone"
                        stroke={zone.color}
                        fill={zone.color}
                        stackId="1"
                    />
                ))}
                <Legend
                    layout="vertical"
                    verticalAlign="middle"
                    align="right"
                    wrapperStyle={{ paddingLeft: 10 }}
                />
            </g>
        </AreaChart>

    );
}

export default ZonesChart;