import {Area, AreaChart, CartesianGrid, LabelList, Legend, Tooltip, XAxis, YAxis} from 'recharts';

import type {ZonesChartProps} from "../../types/zones-chart-props.ts";
import {useGetFilteredMetricsQuery} from "../../storage/services/metrics-api.ts"
import calculateVisualDataForZoneChart from "../../helpers/calculateVisualDataForZoneChart.ts";

function ZonesChart({teamId, dayCount, filter, zoneColor}: ZonesChartProps) {

  const {data, isLoading, isError}
    = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

  if (isLoading) {
    return <h1>Loading...</h1>;
  }
  if (isError) {
    return <h1>Error...</h1>;
  }

  const visualData = calculateVisualDataForZoneChart(data, zoneColor);

  return (
        <AreaChart
          style={{width: '80%', aspectRatio: 1.618, maxHeight: '40vh'}}
          responsive
          data={visualData}
          stackOffset="expand"
          margin={{top: 10, right: 20, left: 0, bottom: 0}}
        >
          <CartesianGrid strokeDasharray="3 3"/>
          <XAxis dataKey="day" orientation="bottom"/>
          <YAxis width="auto" tick={false} />
          <Tooltip
            formatter={(_value, name, props) => {
              const originalValue = props.payload[`original_${name}`];
              return `${originalValue}`;
            }}
          />
          {zoneColor?.reverse().map((zone) => (
            <Area
              key={zone.name}
              dataKey={zone.name}
              type="monotone"
              stroke={zone.color}
              fill={zone.color}
              stackId="1"
            >
              <LabelList
                dataKey={`original_${zone.name}`}
                position="center"
                style={{fontSize: '12px', fill: '#333'}}
                content={(props) => {
                  const {x, y, value} = props;
                  if (value === 0) return null;
                  return (
                    <text x={x} y={Number(y) + 15} textAnchor="middle" fill="#666" fontSize={14}>
                      {value}
                    </text>
                  );
                }}
              />
            </Area>
          ))}
          <Legend
            layout="horizontal"
            verticalAlign="bottom"
            align="center"
            wrapperStyle={{paddingLeft: 10}}
          />
        </AreaChart>
  );
}

export default ZonesChart;