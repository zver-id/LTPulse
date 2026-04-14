export interface ZonesChartProps {
  teamId: number;
  dayCount: number;
  filter: string;
  zoneColor: zonesColor[];
}

export type zonesColor = {
  name: string;
  color: string;
}