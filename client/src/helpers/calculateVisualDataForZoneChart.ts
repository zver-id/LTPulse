import type {TeamData} from "../types/team-data.ts";
import type {zonesColor} from "../types/zones-chart-props.ts";

export interface VisualDataForZoneChart {
  day: string;
  [key: string]: number;
}

function calculateVisualDataForZoneChart(data: TeamData[], zonesColor: zonesColor[]) {
  const MAX_HEIGHT = 0.4;
  const resultArray: VisualDataForZoneChart[] = [];

  data.forEach(teamData => {
    let totalValue = Object.entries(teamData).reduce((acc: number, [, value]) => {
      return acc + (typeof value === "number" ? value : 0);
    }, 0) * MAX_HEIGHT;

    const result: VisualDataForZoneChart = {
      day: teamData.day as string,  // Добавляем day
      ...zonesColor.reduce<Record<string, number>>((acc, { name }) => {
        acc[`original_${name}`] = teamData[name];
        const currentValue = teamData[name] as number;

        if (totalValue === 0) {
          acc[`${name}`] = 0;
        } else if (totalValue > currentValue) {
          acc[`${name}`] = currentValue;
          totalValue -= currentValue;
        } else {
          acc[`${name}`] = totalValue;
          totalValue = 0;
        }
        return acc;
      }, {} as Record<string, number>)
    };

    resultArray.push(result);
  });

  return resultArray;
}

export default calculateVisualDataForZoneChart;