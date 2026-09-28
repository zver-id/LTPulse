import {createApi, fetchBaseQuery} from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type {IEmployeeStats} from "../../components/employeeStats/employeeStats.tsx";

export const employeeStatsApi = createApi({
  reducerPath: "employeeStats",
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/EmployeeStats`}),
  tagTypes: ['employeeStats'],
  endpoints: (builder) => ({
    getEmployeeStats: builder.query<IEmployeeStats[], { teamId: number, beginDate: string, endDate: string }>({
      query: ({teamId, beginDate, endDate}) =>
        `?teamId=${teamId}&beginDate=${beginDate}&endDate=${endDate}`,
      providesTags: ['employeeStats']
    })
  })
});

export const {useGetEmployeeStatsQuery} = employeeStatsApi;
