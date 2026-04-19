import {CartesianGrid, LabelList, Legend, Line, LineChart, Tooltip, XAxis, YAxis} from "recharts";
import type {LineChartProps} from "../../types/monthLineChart.ts";
import {useGetFilteredMetricsQuery} from "../../storage/services/metrics-api.ts"
import getAllLines from "../../helpers/getAllLines.ts"
import loadingOrErrorScreen from "../../helpers/loadingOrErrorScreen.tsx"
import getMaxValue from "../../helpers/getMaxValue.ts";
import {Collapse} from "antd";

function LineTicketChart({teamId, dayCount, filter, nameOfChart}: LineChartProps) {

  const {data, isLoading, isError}
    = useGetFilteredMetricsQuery({teamId: teamId, dayCount: dayCount, filter: filter});

  const loadingOrErrorResult = loadingOrErrorScreen({isLoading, isError})
  if (loadingOrErrorResult) {
    return loadingOrErrorResult;
  }

  if (data?.length === 0) {
    return
  }

  const allMonths = getAllLines(data)
  const yAxisMax = data ? Math.ceil(getMaxValue(data) * 1.1) : 0

  const colors = ['#E6194B', '#3CB44B', '#4363D8', '#F58231', '#911EB4', '#42D4F4',
    '#F032E6', '#BFEF45', '#FABED4', '#469990', '#DCBEFF', '#9A6324', '#800000'];

  return (
    <Collapse defaultActiveKey={['1']} key={1}
              items={[
                {
                  key: 1,
                  label: (
                    <div>{nameOfChart}</div>
                  ),
                  children: (
                    <LineChart
                      style={{width: '80%', aspectRatio: 1.618, maxHeight: '40vh'}}
                      margin={{top: 5, right: 20, bottom: 5, left: 0}}
                      responsive
                      data={data}>

                      <XAxis dataKey="day"/>
                      <YAxis width="auto"
                             domain={[0, yAxisMax > 0 ? yAxisMax : 1]}/>
                      <CartesianGrid stroke="#aaa" strokeDasharray="5 5"/>
                      {allMonths?.map((month, index) => {
                        return (
                          <Line key={month}
                                dataKey={month}
                                stroke={colors[index % colors.length]}
                                type="monotone"
                          >
                            <LabelList
                              dataKey={month}
                              position="top"
                              style={{fontSize: '12px', fill: '#333'}}
                              content={(props) => {
                                const {x, y, value} = props;
                                if (value === 0) return null;
                                return (
                                  <text x={x} y={Number(y) - 8} textAnchor="middle" fill="#666" fontSize={14}>
                                    {value}
                                  </text>
                                );
                              }}
                            />
                          </Line>
                        )
                      })}
                      <Legend/>
                      <Tooltip/>
                    </LineChart>
                  )
                }
              ]}>
    </Collapse>
  );
}

export default LineTicketChart