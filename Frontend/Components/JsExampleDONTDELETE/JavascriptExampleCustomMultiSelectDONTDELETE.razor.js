// Initialize function, create initial tokens with itens that are already selected by the user

let counter = 0;

export function initComponent(dotNetRef, id){
    counter++;
    console.log("init execution counter:", counter);


    const transformToMultiSelect = function (elem) {
        let optionsLabel = elem.getAttribute("optionsLabel")
            ? elem.getAttribute("optionsLabel").split(",")
            : [];
        let optionsValue = elem.getAttribute("optionsValue")
            ? elem.getAttribute("optionsValue").split(",")
            : [];
        let value = elem.value.split(",");

        let parent = document.createElement("DIV");
        parent.className = "ms-inp-parent";
        parent.tabIndex = elem.tabIndex | -1;

        let inp = document.createElement("INPUT");
        inp.type = "text";
        inp.className = "ms-inp-element";
        inp.setAttribute("id", elem.id);
        inp.setAttribute("name", elem.name);
        inp.setAttribute("placeholder", elem.placeholder);
        inp.readOnly = true;
        inp.value = elem.value;

        let optionList = document.createElement("DIV");
        optionList.className = "ms-inp-optionList";

        const  makeSelection =  function () {
            let selection = Array.prototype.slice
                .call(parent.querySelectorAll('input[type="checkbox"]:checked'))
                .map((c) =>
                    keyc.value
                );
            console.log(selection);
            dotNetRef.invokeMethodAsync("OnElementSelected",selection);

            parent.querySelector(".ms-inp-element").value = selection.length
                ? selection.join(",")
                : "";
        };

        for (let i = 0; i < optionsValue.length; i++) {
            let label = document.createElement("LABEL");
            let input = document.createElement("INPUT");
            input.type = "checkbox";
            input.id = elem.name + "_" + i;
            input.value = i < optionsValue.length ? optionsValue[i] : "";
            input.checked = value.includes(optionsValue[i]);

            input.onchange = makeSelection;
            label.appendChild(input);
            label.appendChild(
                document.createTextNode(
                    i < optionsLabel.length ? optionsLabel[i] : optionsValue[i]
                )
            );
            optionList.appendChild(label);
        }

        parent.appendChild(inp);
        parent.appendChild(optionList);

        elem.replaceWith(parent);
    };

    let input = document.getElementById(id);
    transformToMultiSelect(input);

}


