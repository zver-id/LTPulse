import {Button, Flex} from "antd";
import styles from "./metricDetails.module.css"
import {useState} from "react";
import MetricDetailsModal from "../metricDetailsModal/metricDetailsModal.tsx";

export interface IMetricDetailsItem {
  title: string;
  teamId: number;
}

interface IMetricDetailsItems {
  items: IMetricDetailsItem[];
}

function MetricDetails({items}: IMetricDetailsItems) {
  const [openModal, setOpenModal] = useState<number | null>(null);
  const onClickHandle = (index: number) => {
    setOpenModal(index)
  }
  const onClose = () => {
    setOpenModal(null)
  }
  const today = new Date().toISOString().split('T')[0];

  return (
      <Flex className="ant-flex-vertical">
        {items.map((item, index) => {
          const isOpen = openModal === index
          return (
              <>
                <Button
                    className={styles.button}
                    onClick={()=>onClickHandle(index)}
                    key={index}
                >{item.title}</Button>
                {isOpen && (
                    <MetricDetailsModal
                        teamId={item.teamId}
                        date={today}
                        metric={item.title}
                        isOpen={isOpen}
                        onClose={onClose}/>
                )}
              </>
          )
        })}
      </Flex>
  )
}

export default MetricDetails;