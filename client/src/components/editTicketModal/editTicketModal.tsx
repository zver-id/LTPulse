import React, { useEffect } from 'react';
import { Modal, Form, Input, message } from 'antd';
import type {ITicket} from '../ticketsScreen/ticketScreen.tsx';
import TextArea from "antd/es/input/TextArea";
import { useUpdateTicketMutation } from "../../storage/services/tickets-api.ts"

interface EditUserModalProps {
    isOpen: boolean;
    ticket: ITicket | null;
    loading: boolean;           // Состояние загрузки при сохранении
    onClose: () => void;        // Функция закрытия окна
    onSuccess: () => void;      // Функция успешного сохранения (обновляет таблицу)
}

const EditTicketModal: React.FC<EditUserModalProps> = ({ isOpen, ticket, loading, onClose, onSuccess}) => {

    const [ updateTicket ] = useUpdateTicketMutation();
    const [form] = Form.useForm();

    useEffect(() => {
        if (ticket && isOpen) {
            form.setFieldsValue({ ...ticket });
        } else if (!isOpen) {
            form.resetFields();
        }
    }, [ticket, isOpen, form]);

    const handleSubmit = async () => {
        try {
            const formValues = await form.validateFields();
            const values = { ...ticket, ...formValues };

            await updateTicket(values).unwrap();
            message.success('Комментарий успешно обновлен');

            onSuccess(); // Обновляем таблицу
            onClose();   // Закрываем окно

        } catch (error) {
            message.error(`Не удалось сохранить изменения ${error}`);
        }
    };

    return (
        <Modal
            title={`Обращение ${ticket?.key}`}
            open={isOpen}
            onOk={handleSubmit}
            onCancel={onClose}
            confirmLoading={loading}
            okText="Сохранить"
            cancelText="Отмена"
            width={600}
            destroyOnHidden
        >
            <Form
                form={form}
                layout="vertical"
                name="editTicketForm"
            >
                <Form.Item name="name" label="Описание">
                    <Input disabled />
                </Form.Item>

                <Form.Item name="organization" label="Организация">
                    <Input disabled />
                </Form.Item>

                <Form.Item name="comment" label="Комментарий">
                    <TextArea autoSize/>
                </Form.Item>

            </Form>
        </Modal>
    );
};

export default EditTicketModal;