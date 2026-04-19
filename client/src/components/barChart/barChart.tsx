import {useGetFilteredMetricsQuery} from "../../storage/services/metrics-api.ts";
import loadingOrErrorScreen from "../../helpers/loadingOrErrorScreen.tsx";
import {Bar, BarChart, CartesianGrid, LabelList, Legend, Tooltip, XAxis, YAxis} from "recharts";
import {RechartsDevtools} from '@recharts/devtools';
import getAllLines from "../../helpers/getAllLines.ts";
import {Collapse} from "antd";
import getMaxValue from "../../helpers/getMaxValue.ts";

interface IBarChartProps {
  teamId: number;
  dayCount: number;
  filter: string;
  nameOfChart: string;
}

function MetricBarChart({teamId, dayCount, filter, nameOfChart}: IBarChartProps) {

  const {data, isLoading, isError}
    = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

  const loadingOrErrorResult = loadingOrErrorScreen({isLoading, isError})
  if (loadingOrErrorResult) {
    return loadingOrErrorResult;
  }

  const categories = getAllLines(data)
  const yAxisMax = data ? Math.ceil(getMaxValue(data) * 1.1) : 0

  return (
    <Collapse defaultActiveKey={['1']} key={1}
              items={[
                {
                  key: 1,
                  label: (
                    <div>{nameOfChart}</div>
                  ),
                  children: (
                    <BarChart
                      style={{width: '80%', aspectRatio: 1.618, maxHeight: '40vh'}}
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
                      <YAxis width="auto"
                             domain={[0, yAxisMax > 0 ? yAxisMax : 1]}/>
                      <Tooltip/>
                      <Legend/>
                      {
                        categories?.map((category) => {
                          return (
                            <Bar
                              key={category}
                              dataKey={category} fill="#8884d8" activeBar={{fill: 'pink', stroke: 'blue'}}>
                              <LabelList
                                dataKey={category}
                                position="top"
                                style={{fontSize: '12px', fill: '#333'}}
                                content={(props) => {
                                  const {x, y, value, width} = props;
                                  if (value === 0) return null;
                                  return (
                                    <text x={Number(x) + Number(width) / 2}
                                          y={Number(y) - 8}
                                          textAnchor="middle" fill="#666" fontSize={14}>
                                      {value}
                                    </text>
                                  );
                                }}
                              />
                            </Bar>
                          )
                        })
                      }
                      <RechartsDevtools/>
                    </BarChart>
                  )
                }
              ]}>
    </Collapse>

  )
}

export default MetricBarChart