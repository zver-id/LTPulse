import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import apiUrl from "./api-url.ts";

export interface IEmployee {
  id: number;
  name: string;
}

export const employeeApi = createApi({
  reducerPath: 'employee',
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/Employee`}),
  tagTypes: ["employees"],

  endpoints: (builder) => ({
    getEmployeesByTeam: builder.query<IEmployee[], { teamId: number }>({
      query: ({ teamId }) => `?teamId=${teamId}`,
      providesTags: ['employees']
    }),
  })
})

export const {
  useGetEmployeesByTeamQuery
} = employeeApi;
