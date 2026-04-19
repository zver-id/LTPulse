import {Area, AreaChart, CartesianGrid, LabelList, Legend, Tooltip, XAxis, YAxis} from 'recharts';

import type {ZonesChartProps, zonesColor} from "../../types/zones-chart-props.ts";
import {useGetFilteredMetricsQuery} from "../../storage/services/metrics-api.ts"
import calculateVisualDataForZoneChart from "../../helpers/calculateVisualDataForZoneChart.ts";
import {Collapse} from "antd";

function ZonesChart({teamId, dayCount, filter, nameOfChart}: ZonesChartProps) {

  const {data, isLoading, isError}
    = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

  const colors = ['#3CB44B', '#F5DEB3', '#FFD700', '#E6194B'];
  const zoneColorSimple: zonesColor[] = [
    {name: '>24', color: colors[3]},
    {name: '16-24', color: colors[2]},
    {name: '8-16', color: colors[1]},
    {name: '0-8', color: colors[0]}
  ]

  const zoneColorPriority: zonesColor[] = [
    {name: '>0.75', color: colors[3]},
    {name: '0.5-0.75', color: colors[2]},
    {name: '0.25-0.5', color: colors[1]},
    {name: '<0.25', color: colors[0]}
  ]

  const zoneColor: zonesColor[] = filter === 'ColorZones' ? zoneColorSimple : zoneColorPriority;
  const isHidden: string[] = filter === 'ColorZones' ? ['1'] : [];

  if (isLoading) {
    return <h1>Loading...</h1>;
  }
  if (isError) {
    return <h1>Error...</h1>;
  }

  const visualData = calculateVisualDataForZoneChart(data ?? [], zoneColor);

  return (
    <Collapse defaultActiveKey={isHidden} key={1}
              items={[
                {
                  key: 1,
                  label: (
                    <div>{nameOfChart}</div>
                  ),
                  children:(
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
                  )
                }
              ]}>
    </Collapse>

  );
}

export default ZonesChart;