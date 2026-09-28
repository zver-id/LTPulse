import {createApi, fetchBaseQuery} from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type {Team} from "../../types/team.ts";

export const teamsApi = createApi({
  reducerPath: "teams",
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api`}),
  tagTypes: ["teams"],
  endpoints: (builder) => ({
    getAllTeams: builder.query<Team[], void>({
      query: () => `teams`,
      providesTags: ["teams"],
    }),

    recalculateMetrics: builder.mutation<void, number>({
      query: (teamId) => ({
        url: `teams/${teamId}/recalculate`,
        method: "POST",
      }),
      // Публикует сообщение в RabbitMQ. Остальные кэши (metrics, tickets, grades)
      // сбрасываются из компонента, т.к. RTK Query не позволяет инвалидировать
      // теги другого API из мутации.
      invalidatesTags: ["teams"],
    })
  })
});

export const {useGetAllTeamsQuery, useRecalculateMetricsMutation} = teamsApi;
